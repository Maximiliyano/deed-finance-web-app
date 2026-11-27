import { UserSettings } from '../../home/models/user-settings.model';

export interface User {
    id: string;
    fullname: string;
    email: string;
    emailVerified: boolean;
    pictureUrl: string;
    roles: string[];
    settings: UserSettings | null;
}
