import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { deserialize } from 'santee-dcts';
import { AppSettings } from './app-settings';
import { AppComponent } from './app.component';
import { DashboardModule } from './dashboard/dashboard.module';

const field = document.querySelector<HTMLInputElement>('#app-settings');

if (!field) {
    throw new Error('#app-settings not found');
}

const appSettings = deserialize(JSON.parse(field.value), AppSettings);

@NgModule({
    declarations: [
        AppComponent,
    ],
    imports: [
        CommonModule,
        BrowserModule,
        DashboardModule,
    ],
    providers: [
        { provide: AppSettings, useValue: appSettings },
    ],
    bootstrap: [AppComponent],
})
export class AppModule { }
