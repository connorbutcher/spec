import { Permission } from '../../../../../../../core/models/domain/access/permission';
import { User } from '../../../../../../../core/models/domain/access/user';

export interface ManagePermissionsDialogData {
  user: User;
  permissionsToManage: Permission[];
}
