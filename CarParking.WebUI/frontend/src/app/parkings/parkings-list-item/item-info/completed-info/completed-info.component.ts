import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CompletedFirst, CompletedFree } from '../../../models';

@Component({
    selector: 'complete-info',
    templateUrl: './completed-info.component.html',
    styleUrls: ['../item-info.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CompletedInfoComponent {
    @Input()
    public parking!: CompletedFree | CompletedFirst;
}
