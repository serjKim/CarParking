import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
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
    ],
})
export class DashboardModule { }
