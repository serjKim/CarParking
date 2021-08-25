import { FormControl } from '@angular/forms';
import { TransitionName } from '../models';

export class TransitionButton {
    public get buttonId(): string {
        return `${this.transitionName}-status-button`;
    }

    constructor(
        public readonly transitionName: TransitionName,
        public readonly control: FormControl,
        public readonly style: ButtonStyle,
    ) { }
}

export interface ButtonStyle {
    readonly icon: string;
    readonly className: string;
}

export type ButtonStyles = {
    readonly [key in TransitionName]: ButtonStyle;
};
