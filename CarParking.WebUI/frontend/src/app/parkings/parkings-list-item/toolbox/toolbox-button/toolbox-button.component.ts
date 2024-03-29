import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { ToolboxButtonType } from '../toolbox';

type StylesByType = {
    [key in ToolboxButtonType]: {
        readonly iconName: string;
        readonly className: string;
    };
};

@Component({
    selector: 'toolbox-button',
    templateUrl: './toolbox-button.component.html',
    styleUrls: ['./toolbox-button.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToolboxButtonComponent {
    @Output()
    public click = new EventEmitter<void>();

    @Input()
    public disabled: boolean | null = false;

    @Input()
    public buttonType = ToolboxButtonType.Complete;

    public readonly stylesByType: StylesByType = {
        [ToolboxButtonType.Complete]: { iconName: 'check_circle', className: 'complete' },
        [ToolboxButtonType.Pay]: { iconName: 'attach_money', className: 'pay' },
    };

    public onClick(e: MouseEvent) {
        e.stopPropagation();

        if (this.disabled) {
            return;
        }

        this.click.emit();
    }
}
