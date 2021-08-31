import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { TransitionButtonsComponent } from './transition-buttons.component';

@NgModule({
    imports: [
        CommonModule,
        ReactiveFormsModule,
    ],
    declarations: [
        TransitionButtonsComponent,
    ],
    exports: [
        TransitionButtonsComponent,
    ],
    providers: [],
})
export class TransitionButtonsModule { }
