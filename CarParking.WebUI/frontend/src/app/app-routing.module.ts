import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';

const routes: Routes = [
    {
        path: '',
        component: DashboardComponent,
        children: [
            {
                path: 'parkings',
                loadChildren: () => import('./parkings').then((m) => m.ParkingsModule),
            },
            {
                path: '',
                pathMatch: 'full',
                redirectTo: '/parkings',
            },
        ],
    },
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule],
})
export class AppRoutingModule { }
