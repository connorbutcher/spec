import tseslint from 'typescript-eslint';

/**
 * Lint policy for the app:
 *  - Braces are always required around control-flow bodies (`curly: all`).
 *  - One class per file, and one exported interface per file.
 *  - Components use templateUrl/styleUrl, never inline templates or styles.
 *  - Every class member states its accessibility (`public` / `private`; `protected` is not used).
 *  - Members are ordered fields, constructor, then methods/getters, public before private.
 *
 * Only `.ts` sources are linted; templates are not.
 */
export default tseslint.config(
  {
    ignores: ['dist/**', 'node_modules/**', '.angular/**'],
  },
  {
    files: ['src/**/*.ts'],
    ignores: ['src/**/*.spec.ts'],
    languageOptions: { parser: tseslint.parser },
    plugins: { '@typescript-eslint': tseslint.plugin },
    rules: {
      curly: ['error', 'all'],
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
      'no-restricted-syntax': [
        'error',
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
          selector: 'Program > :matches(TSInterfaceDeclaration, ExportNamedDeclaration > TSInterfaceDeclaration) ~ :matches(TSInterfaceDeclaration, ExportNamedDeclaration > TSInterfaceDeclaration)',
          message: 'One interface per file.',
        },
      ],
    },
  },
);
