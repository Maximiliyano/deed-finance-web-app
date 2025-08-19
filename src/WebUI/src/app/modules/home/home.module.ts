import { HomeComponent } from './home.component';
import { HomeRoutingModule } from './home-routing.module';
import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { SocialListComponent } from './social-list/social-list.component';
import { NewRequestDialogComponent } from './social-list/new-request-dialog/new-request-dialog.component';

@NgModule({
  declarations: [
    HomeComponent,
    SocialListComponent,
    NewRequestDialogComponent
  ],
  imports: [HomeRoutingModule, SharedModule]
})
export class HomeModule { }
