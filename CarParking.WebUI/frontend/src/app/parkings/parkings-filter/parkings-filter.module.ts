import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { ParkingsFilterComponent } from './parkings-filter.component';
import { TransitionButtonsComponent } from './transition-buttons';

@NgModule({
    imports: [
        CommonModule,
        ReactiveFormsModule,
    ],
    declarations: [
        ParkingsFilterComponent,
        TransitionButtonsComponent,
    ],
    exports: [
        ParkingsFilterComponent,
    ],
    providers: [],
})
export class ParkingsFilterModule { }
