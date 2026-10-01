/**
 * Native form controls must be PrimeNG controls. A native element is allowed only when it carries the
 * PrimeNG directive that styles it (`<button pButton>`, `<input pInputText>`, `<textarea pTextarea>`);
 * otherwise the PrimeNG component is used instead.
 */
const INPUT_TYPE_REPLACEMENTS = {
  checkbox: '<p-checkbox>',
  radio: '<p-radiobutton>',
  number: '<p-inputnumber>',
  date: '<p-datepicker>',
  'datetime-local': '<p-datepicker>',
  time: '<p-datepicker>',
  color: '<p-colorpicker>',
  file: '<p-fileupload>',
  range: '<p-slider>',
  password: '<p-password>',
};

function attributeNames(element) {
  return [...element.attributes, ...element.inputs].map((attribute) => attribute.name);
}

function inputType(element) {
  return element.attributes.find((attribute) => attribute.name === 'type')?.value ?? 'text';
}

export default {
  meta: {
    type: 'suggestion',
    docs: { description: 'Use PrimeNG controls instead of native form controls.' },
    schema: [],
    messages: {
      usePrimeNg: 'Use {{replacement}} instead of a native <{{element}}>.',
    },
  },
  create(context) {
    const parserServices = context.sourceCode.parserServices;

    function report(element, replacement) {
      context.report({
        loc: parserServices.convertNodeSourceSpanToLoc(element.startSourceSpan),
        messageId: 'usePrimeNg',
        data: { element: element.name, replacement },
      });
    }

    return {
      'Element[name=/^(button|input|textarea|select|table)$/]'(element) {
        const names = attributeNames(element);

        if (element.name === 'button' && !names.includes('pButton')) {
          report(element, '<p-button> or <button pButton>');
        }
        if (element.name === 'textarea' && !names.includes('pTextarea')) {
          report(element, '<textarea pTextarea>');
        }
        if (element.name === 'select') {
          report(element, '<p-select>');
        }
        if (element.name === 'table') {
          report(element, '<p-table>');
        }
        if (element.name === 'input') {
          const type = inputType(element);
          if (type === 'hidden') {
            return;
          }
          const replacement = INPUT_TYPE_REPLACEMENTS[type];
          if (replacement) {
            report(element, replacement);
          } else if (!names.includes('pInputText')) {
            report(element, '<input pInputText>');
          }
        }
      },
    };
  },
};
