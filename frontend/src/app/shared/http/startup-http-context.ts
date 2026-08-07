import { HttpContextToken } from '@angular/common/http';

export const IS_STARTUP_REQUEST = new HttpContextToken<boolean>(() => false);