import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { ParkingsListItemComponent } from './parkings-list-item/parkings-list-item.component';
import { ParkingsListComponent } from './parkings-list/parkings-list.component';
import { ParkingsApi } from './parkings.api';

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
        HttpClientModule,
    ],
    providers: [
        ParkingsApi,
    ],
})
export class ParkingsModule { }
