import { Sheet } from './sheet';
import { SheetSubmission } from './sheet-submission';

export type SheetDetail = {
  sheet: Sheet;
  submissions: SheetSubmission[];
};
