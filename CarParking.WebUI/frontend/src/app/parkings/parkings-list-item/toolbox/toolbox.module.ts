import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ToolboxButtonComponent } from './toolbox-button/toolbox-button.component';
import { ToolboxComponent } from './toolbox.component';

@NgModule({
    imports: [
        CommonModule,
    ],
    declarations: [
        ToolboxButtonComponent,
        ToolboxComponent,
    ],
    exports: [
        ToolboxComponent,
    ],
    providers: [],
})
export class ToolboxModule { }
