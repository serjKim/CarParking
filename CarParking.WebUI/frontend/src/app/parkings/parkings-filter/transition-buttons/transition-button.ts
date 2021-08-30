import { FormControl } from '@angular/forms';
import { TransitionName } from '../../models';

export interface ButtonStyle {
    readonly icon: string;
    readonly className: string;
}

export type ButtonStyles = {
    readonly [key in TransitionName]: ButtonStyle;
};

export class TransitionButton {
    public readonly buttonId: string;

    constructor(
        public readonly transitionName: TransitionName,
        public readonly control: FormControl,
        public readonly style: ButtonStyle,
    ) {
        this.buttonId = `${this.transitionName}-status-button`;
    }
}
