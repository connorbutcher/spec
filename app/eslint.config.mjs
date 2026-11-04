import stylistic from '@stylistic/eslint-plugin';
import angular from 'angular-eslint';
import tseslint from 'typescript-eslint';
import project from './eslint-rules/index.mjs';

const COMPONENT_CLASS = "ClassDeclaration:has(Decorator[expression.callee.name='Component'])";
const FUNCTION_VALUE = ':matches(ArrowFunctionExpression, FunctionExpression)';

// Angular's functional APIs (HttpInterceptorFn, CanActivateFn, ...) are typed constants, so a top-level
// function value is allowed only when its type is declared.
const STANDALONE_FUNCTIONS = [
  'Program > FunctionDeclaration',
  'Program > ExportNamedDeclaration > FunctionDeclaration',
  'Program > ExportDefaultDeclaration > FunctionDeclaration',
  `Program > VariableDeclaration > VariableDeclarator:not([id.typeAnnotation]) > ${FUNCTION_VALUE}`,
  `Program > ExportNamedDeclaration > VariableDeclaration > VariableDeclarator:not([id.typeAnnotation]) > ${FUNCTION_VALUE}`,
].join(', ');

const LOCAL_FUNCTIONS = [':function FunctionDeclaration', `:function VariableDeclarator > ${FUNCTION_VALUE}`].join(', ');

const ONE_INTERFACE = 'Program > :matches(TSInterfaceDeclaration, ExportNamedDeclaration > TSInterfaceDeclaration) ~ :matches(TSInterfaceDeclaration, ExportNamedDeclaration > TSInterfaceDeclaration)';

const SHARED_SYNTAX = [
  {
    selector: STANDALONE_FUNCTIONS,
    message: 'Put helpers in a class as static methods, not standalone functions.',
  },
  {
    selector: LOCAL_FUNCTIONS,
    message: 'Do not declare functions inside functions; make it a method (private static for helpers).',
  },
  {
    selector: ONE_INTERFACE,
    message: 'One interface per file.',
  },
];

/**
 * Lint policy for the app:
 *  - Braces are always required around control-flow bodies (`curly: all`), opening on the statement's line (1TBS).
 *  - One class per file, and one interface per file.
 *  - Helpers are static methods on a class, never standalone functions, and no function is declared inside another.
 *  - Every component lives in `<name>/<name>.ts` with `<name>.html` and `<name>.scss` beside it, never
 *    inline templates or styles, never ".component" in file names or "Component" on class names.
 *  - Signals for state, inputs, outputs and queries; `httpResource` for reads; no `subscribe` in components.
 *  - PrimeNG controls instead of native form controls in templates.
 *  - Every class member states its accessibility (`public` / `private`; `protected` is not used).
 *  - Members are ordered fields, constructor, then methods/getters, public before private.
 *
 * Specs follow the first three. Run with `npm run lint`; `npm run lint:fix` applies the automatic fixes.
 */
export default tseslint.config(
  {
    ignores: ['dist/**', 'node_modules/**', '.angular/**'],
  },
  {
    files: ['src/**/*.spec.ts'],
    languageOptions: { parser: tseslint.parser },
    plugins: { '@stylistic': stylistic },
    rules: {
      curly: ['error', 'all'],
      '@stylistic/brace-style': ['error', '1tbs'],
      '@stylistic/no-trailing-spaces': 'error',
      'max-classes-per-file': ['error', 1],
      'no-restricted-syntax': ['error', ...SHARED_SYNTAX],
    },
  },
  {
    files: ['src/**/*.ts'],
    ignores: ['src/**/*.spec.ts'],
    extends: [...angular.configs.tsRecommended],
    processor: angular.processInlineTemplates,
    languageOptions: { parser: tseslint.parser },
    plugins: { '@typescript-eslint': tseslint.plugin, '@stylistic': stylistic, project },
    rules: {
      curly: ['error', 'all'],
      '@stylistic/brace-style': ['error', '1tbs'],
      '@stylistic/no-trailing-spaces': 'error',
      'max-classes-per-file': ['error', 1],
      '@typescript-eslint/explicit-member-accessibility': [
        'error',
        { accessibility: 'explicit', overrides: { constructors: 'no-public' } },
      ],
      '@typescript-eslint/member-ordering': [
        'error',
        {
          default: [
            'public-static-field',
            'private-static-field',
            'public-instance-field',
            'private-instance-field',
            'constructor',
            'public-static-method',
            'private-static-method',
            'public-instance-method',
            'public-instance-get',
            'public-instance-set',
            'private-instance-method',
            'private-instance-get',
            'private-instance-set',
          ],
        },
      ],

      // Component classes have no "Component" suffix; the naming check lives in no-restricted-syntax.
      '@angular-eslint/component-class-suffix': 'off',
      '@angular-eslint/component-selector': ['error', { type: 'element', prefix: 'app', style: 'kebab-case' }],
      '@angular-eslint/directive-selector': ['error', { type: 'attribute', prefix: 'app', style: 'camelCase' }],
      '@angular-eslint/component-max-inline-declarations': ['error', { template: 0, styles: 0, animations: 0 }],
      '@angular-eslint/prefer-standalone': 'error',
      '@angular-eslint/prefer-inject': 'error',
      '@angular-eslint/prefer-signals': 'error',
      '@angular-eslint/prefer-output-emitter-ref': 'error',
      'project/component-file-layout': 'error',

      'no-restricted-syntax': [
        'error',
        ...SHARED_SYNTAX,
        {
          selector: "[accessibility='protected']",
          message: 'Use public or private, not protected.',
        },
        {
          selector:
            "Decorator[expression.callee.name='Component'] Property[key.name=/^(template|styles)$/]",
          message: 'Put component markup and styles in their own .html and .scss files.',
        },
        {
          selector: `${COMPONENT_CLASS} > Identifier.id[name=/Component$/]`,
          message: 'Do not suffix component class names with "Component".',
        },
        {
          selector: `${COMPONENT_CLASS} CallExpression[callee.property.name='subscribe']`,
          message: 'Do not subscribe in components; read data through a resource and expose it as signals.',
        },
        {
          selector: "CallExpression[callee.object.property.name='http'][callee.property.name='get']",
          message: 'Read data with httpResource rather than HttpClient.get.',
        },
      ],
    },
  },
  {
    files: ['src/**/*.html'],
    extends: [...angular.configs.templateRecommended],
    plugins: { project },
    rules: {
      '@angular-eslint/template/prefer-control-flow': 'error',
      '@angular-eslint/template/button-has-type': 'error',
      'project/primeng-controls': 'error',
    },
  },
);
