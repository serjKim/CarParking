import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { ParkingsFilterComponent } from './parkings-filter.component';

@NgModule({
    imports: [
        CommonModule,
        ReactiveFormsModule,
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
