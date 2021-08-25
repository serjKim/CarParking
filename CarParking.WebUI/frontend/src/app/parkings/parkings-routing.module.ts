import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ParkingsComponent } from './parkings/parkings.component';

const routes: Routes = [
    {
        path: '',
        component: ParkingsComponent,
    },
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule],
})
export class ParkingsRoutingModule { }
