import { NgModule } from '@angular/core';
import { ParkingsModule } from '../parkings/parkings.module';
import { CreateParkingButtonComponent } from './create-parking-button/create-parking-button.component';
import { DashboardComponent } from './dashboard.component';

@NgModule({
    declarations: [
        DashboardComponent,
        CreateParkingButtonComponent,
    ],
    exports: [
        DashboardComponent,
    ],
    imports: [
        ParkingsModule,
    ],
})
export class DashboardModule { }
