import { existsSync } from 'node:fs';
import { basename, dirname, join } from 'node:path';

/**
 * Every component lives in its own folder named after the file, as `<name>/<name>.ts`, with its
 * markup and styles beside it in `<name>.html` and `<name>.scss`. File names never contain
 * `.component`.
 */
export default {
  meta: {
    type: 'suggestion',
    docs: { description: 'Enforce the one-folder, three-file layout for components.' },
    schema: [],
    messages: {
      componentInFileName: 'Do not use ".component" in file names; name the file "{{expected}}".',
      folderName: 'A component file must live in a folder of the same name ("{{name}}/{{name}}.ts").',
      templateUrl: 'Set templateUrl to "./{{name}}.html".',
      styleUrl: 'Set styleUrl to "./{{name}}.scss".',
      missingFile: 'Missing "{{file}}" next to this component.',
    },
  },
  create(context) {
    const filename = context.filename;
    const file = basename(filename);
    const folder = dirname(filename);
    const name = file.replace(/\.ts$/, '');

    function readString(property) {
      if (property?.value.type === 'Literal' && typeof property.value.value === 'string') {
        return property.value.value;
      }
      return undefined;
    }

    return {
      Program(node) {
        if (file.includes('.component.')) {
          context.report({
            node,
            messageId: 'componentInFileName',
            data: { expected: file.replace('.component.', '.') },
          });
        }
      },
      "Decorator[expression.callee.name='Component'] > CallExpression > ObjectExpression"(node) {
        if (basename(folder) !== name) {
          context.report({ node, messageId: 'folderName', data: { name } });
        }

        const properties = node.properties.filter((p) => p.type === 'Property');
        const templateUrl = properties.find((p) => p.key.name === 'templateUrl');
        const styleUrl = properties.find((p) => p.key.name === 'styleUrl');

        if (readString(templateUrl) !== `./${name}.html`) {
          context.report({ node: templateUrl ?? node, messageId: 'templateUrl', data: { name } });
        }
        if (readString(styleUrl) !== `./${name}.scss`) {
          context.report({ node: styleUrl ?? node, messageId: 'styleUrl', data: { name } });
        }

        for (const extension of ['html', 'scss']) {
          const sibling = `${name}.${extension}`;
          if (!existsSync(join(folder, sibling))) {
            context.report({ node, messageId: 'missingFile', data: { file: sibling } });
          }
        }
      },
    };
  },
};
