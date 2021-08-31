import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ParkingsFilterComponent } from './parkings-filter.component';
import { TransitionButtonsModule } from './transition-buttons';

@NgModule({
    imports: [
        CommonModule,
        TransitionButtonsModule,
    ],
    declarations: [
        ParkingsFilterComponent,
    ],
    exports: [
        ParkingsFilterComponent,
    ],
    providers: [],
})
export class ParkingsFilterModule { }
