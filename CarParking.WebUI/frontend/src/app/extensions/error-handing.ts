export const errorUnhandledType = (obj: never) => {
    console.error('Unhandled type source: ', obj);
    return new Error('Unhandled type.');
};

export const notNullOrFail = <T>(value: T | null | undefined): T => {
    if (value == null) {
        throw new Error('Expected a not null value.');
    }
    return value;
};
