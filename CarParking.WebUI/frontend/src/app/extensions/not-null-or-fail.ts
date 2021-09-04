export const notNullOrFail = <T>(value: T | null | undefined): T => {
    if (value == null) {
        throw new Error('Expected a not null value.');
    }
    return value;
};
