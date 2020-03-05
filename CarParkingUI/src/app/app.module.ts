import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { CreateParkingButtonComponent } from './create-parking-button/create-parking-button.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { ParkingComponent } from './parking/parking.component';
import { ParkingsListComponent } from './parkings-list/parkings-list.component';

@NgModule({
    declarations: [
        AppComponent,
        DashboardComponent,
        ParkingsListComponent,
        ParkingComponent,
        CreateParkingButtonComponent,
    ],
    imports: [
        CommonModule,
        BrowserModule,
        AppRoutingModule,
    ],
    providers: [],
    bootstrap: [AppComponent],
})
export class AppModule { }
