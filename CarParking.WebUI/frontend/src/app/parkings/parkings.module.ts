import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { ParkingsFilterComponent } from './parkings-filter/parkings-filter.component';
import { CompletedInfoComponent } from './parkings-list-item/item-info/completed-info/completed-info.component';
import { StartedInfoComponent } from './parkings-list-item/item-info/started-info/started-info.component';
import { ParkingsListItemComponent } from './parkings-list-item/parkings-list-item.component';
import { CompleteButtonComponent } from './parkings-list-item/toolbox/complete-button/complete-button.component';
import { PayButtonComponent } from './parkings-list-item/toolbox/pay-button/pay-button.component';
import { ToolboxComponent } from './parkings-list-item/toolbox/toolbox.component';
import { ParkingsListComponent } from './parkings-list/parkings-list.component';
import { ParkingsRoutingModule } from './parkings-routing.module';
import { ParkingsComponent } from './parkings/parkings.component';

@NgModule({
    declarations: [
        ParkingsListComponent,
        ParkingsListItemComponent,
        StartedInfoComponent,
        CompletedInfoComponent,
        ToolboxComponent,
        PayButtonComponent,
        ParkingsFilterComponent,
        CompleteButtonComponent,
        ParkingsComponent,
    ],
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ParkingsRoutingModule,
    ],
})
export class ParkingsModule { }
