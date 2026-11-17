export type UpdateActionType = 'none' | 'update' | 'defer';

export type UpdateDialogResult = {
  action: UpdateActionType;
};
