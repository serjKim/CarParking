import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ParkingsListItemComponent } from './parkings-list-item/parkings-list-item.component';
import { ParkingsListComponent } from './parkings-list/parkings-list.component';

@NgModule({
    declarations: [
        ParkingsListComponent,
        ParkingsListItemComponent,
    ],
    exports: [
        ParkingsListComponent,
        ParkingsListItemComponent,
    ],
    imports: [
        CommonModule,
    ],
})
export class ParkingsModule { }
