import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { ParkingsFilterModule } from './parkings-filter';
import { CompletedInfoComponent } from './parkings-list-item/item-info/completed-info/completed-info.component';
import { StartedInfoComponent } from './parkings-list-item/item-info/started-info/started-info.component';
import { ParkingsListItemComponent } from './parkings-list-item/parkings-list-item.component';
import { ToolboxModule } from './parkings-list-item/toolbox';
import { ParkingsListComponent } from './parkings-list/parkings-list.component';
import { ParkingsRoutingModule } from './parkings-routing.module';
import { ParkingsComponent } from './parkings/parkings.component';

@NgModule({
    declarations: [
        ParkingsListComponent,
        ParkingsListItemComponent,
        StartedInfoComponent,
        CompletedInfoComponent,
        ParkingsComponent,
    ],
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ParkingsRoutingModule,
        ToolboxModule,
        ParkingsFilterModule,
    ],
})
export class ParkingsModule { }
