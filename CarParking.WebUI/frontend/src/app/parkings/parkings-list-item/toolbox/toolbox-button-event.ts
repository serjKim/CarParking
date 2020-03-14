import { CompletionResultType } from '../../models/completion';

export const enum ToolboxButtonEventType {
    Completion,
}

export class CompleteButtonEvent {
    public readonly eventType = ToolboxButtonEventType.Completion;
    constructor(public readonly completionResult: CompletionResultType) { }
}

export type ToolboxButtonEvent =
    | CompleteButtonEvent
    ;
