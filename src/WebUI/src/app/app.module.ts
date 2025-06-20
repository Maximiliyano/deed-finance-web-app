import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';
import { HeaderComponent } from './core/layout/header/header.component';
import { FooterComponent } from './core/layout/footer/footer.component';
import { AngularMaterialModule } from './shared/angular-material.module';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { loadingInterceptor } from './core/interceptors/loading.interceptor';
import { provideAnimations } from '@angular/platform-browser/animations';
import { LoadingComponent } from './shared/components/loading/loading.component';

@NgModule({
  declarations: [
    AppComponent,
    HeaderComponent,
    FooterComponent,
    LoadingComponent
  ],
  imports: [
    AppRoutingModule,
    BrowserModule,
    AngularMaterialModule
  ],
  bootstrap: [AppComponent],
  providers: [
    provideHttpClient(
      withInterceptors([errorInterceptor, loadingInterceptor])
    ),
    provideAnimations()
  ]
})
export class AppModule { }
