export interface User {
    fullname: string;
    email: string;
    emailVerified: boolean;
    pictureUrl: string;
    salary?: number;
    currency?: string;
}
