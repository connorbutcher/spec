import componentFileLayout from './component-file-layout.mjs';
import primengControls from './primeng-controls.mjs';

/** Project-specific lint rules that no published plugin covers. */
export default {
  meta: { name: 'pu-spec-sheet' },
  rules: {
    'component-file-layout': componentFileLayout,
    'primeng-controls': primengControls,
  },
};
