export const enum CompletionResultType {
    Success,
    FreeExpired,
}

export interface CompletionResult {
    readonly type: CompletionResultType;
}
