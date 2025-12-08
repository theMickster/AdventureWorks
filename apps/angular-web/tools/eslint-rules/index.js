const { getTemplateParserServices } = require('@angular-eslint/utils');

const FORBIDDEN = /\btext-(?!(?:xs|sm|base|lg|xl|[2-9]xl)\/)[a-zA-Z][\w-]*\/\d{1,3}\b/g;
const MESSAGE =
  'Tailwind opacity-modified text color "{{cls}}" compiles to runtime alpha compositing and ' +
  'cannot be checked for WCAG AA contrast. Use a solid color utility instead (e.g. "text-secondary"). ' +
  'See .claude/rules/alpine-circuit-theming.md.';

const noTextColorOpacity = {
  meta: { type: 'problem', schema: [], messages: { forbidden: MESSAGE } },
  create(context) {
    // angular-eslint's tsRecommended config extracts inline @Component templates into a
    // virtual "<file>.ts/N_inline-template-....component.html" sub-file and lints it through
    // this same '.html' path automatically — so inline templates need no separate handling here.
    if (!context.filename.endsWith('.html')) return {};

    const parserServices = getTemplateParserServices(context);
    return {
      Element(node) {
        const classAttr = node.attributes.find((a) => a.name === 'class');
        if (!classAttr || typeof classAttr.value !== 'string') return;
        for (const match of classAttr.value.matchAll(FORBIDDEN)) {
          context.report({
            loc: parserServices.convertNodeSourceSpanToLoc(classAttr.sourceSpan),
            messageId: 'forbidden',
            data: { cls: match[0] },
          });
        }
      },
    };
  },
};

module.exports = { rules: { 'no-text-color-opacity': noTextColorOpacity } };
