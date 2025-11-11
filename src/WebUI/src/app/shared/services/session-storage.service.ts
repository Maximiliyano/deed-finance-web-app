import { Injectable } from "@angular/core";

@Injectable({
  providedIn: 'root'
})
export class SessionStorageService {
    load<T>(key: string): CachedData<T> | null {
        try {
            const raw = sessionStorage.getItem(key);
            return raw ? JSON.parse(raw) as CachedData<T> : null;
        }
        catch {
            return null;
        }
    }

    save<T>(key: string, data: T): void {
        const payload: CachedData<T> = { data, fetchedAt: Date.now() };
        sessionStorage.setItem(key, JSON.stringify(payload));
    }

    clear(key: string): void {
        sessionStorage.removeItem(key);
    }

    isExpired(fetchedAt: number, targetCacheTime: number): boolean {
        return Date.now() - fetchedAt > targetCacheTime;
    }
}

export interface CachedData<T> {
    data: T;
    fetchedAt: number;
}