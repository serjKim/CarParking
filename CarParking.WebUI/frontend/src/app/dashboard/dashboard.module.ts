import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
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
        RouterModule,
        ParkingsModule,
    ],
})
export class DashboardModule { }
