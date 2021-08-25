import { ChangeDetectionStrategy, Component, Input, Self } from '@angular/core';
import { NotNullProperty } from '../../../extensions';
import { Parking } from '../../models';
import { Toolbox, ToolBoxClick, ToolboxFactory } from './toolbox';

@Component({
    selector: 'toolbox',
    templateUrl: './toolbox.component.html',
    styleUrls: ['./toolbox.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [
        ToolboxFactory,
    ],
})
export class ToolboxComponent {
    @Input()
    @NotNullProperty()
    public set parking(prk: Parking) {
        this.toolbox = this.toolboxFactory.createToolbox(prk.type);
    }

    public toolbox: Toolbox | null = null;

    constructor(
        @Self() private readonly toolboxFactory: ToolboxFactory,
    ) {}

    public onToolButtonClick(handler: ToolBoxClick) {
        if (this.toolbox != null) {
            handler(this.parking, this.toolbox);
        }
    }
}
