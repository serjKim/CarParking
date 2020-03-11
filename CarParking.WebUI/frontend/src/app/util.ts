export const assertUnhandledType = (type: never) => {
    throw new Error(`Unhandled type: ${type}`);
};
