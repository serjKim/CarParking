import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
    selector: 'application',
    styleUrls: ['./app.component.scss'],
    templateUrl: './app.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent { }
