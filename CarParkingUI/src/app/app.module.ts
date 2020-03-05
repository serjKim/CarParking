import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { ParkingsListComponent } from './parkings-list/parkings-list.component';
import { ParkingComponent } from './parking/parking.component';

@NgModule({
    declarations: [
        AppComponent,
        DashboardComponent,
        ParkingsListComponent,
        ParkingComponent,
    ],
    imports: [
        BrowserModule,
        AppRoutingModule,
    ],
    providers: [],
    bootstrap: [AppComponent],
})
export class AppModule { }
