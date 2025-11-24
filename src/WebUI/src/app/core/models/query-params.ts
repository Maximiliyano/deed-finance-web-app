export interface QueryParams {
    searchTerm: string | null;
    sortBy: string | null;
    sortDirection: string | null;
    filterBy: 'onlyForSavings' | null;
}