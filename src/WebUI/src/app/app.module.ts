import {BrowserModule} from '@angular/platform-browser';
import {NgModule} from '@angular/core';

import {AppComponent} from './app.component';
import {AppRoutingModule} from './app-routing.module';
import {HeaderComponent} from './core/layout/header/header.component';
import {FooterComponent} from './core/layout/footer/footer.component';
import {provideHttpClient, withInterceptors} from '@angular/common/http';
import {credentialsInterceptor} from './core/interceptors/credentials.interceptor';
import {errorInterceptor} from './core/interceptors/error.interceptor';
import {retryInterceptor} from './core/interceptors/retry.interceptor';
import {sectionLoadingInterceptor} from './core/interceptors/section-loading.interceptor';
import {provideAnimations} from '@angular/platform-browser/animations';
import {SharedModule} from "./shared/shared.module";
import {NgOptimizedImage} from "@angular/common";

@NgModule({
  declarations: [
    AppComponent,
    HeaderComponent,
    FooterComponent,
  ],
  imports: [
    AppRoutingModule,
    BrowserModule,
    SharedModule,
    NgOptimizedImage
  ],
  bootstrap: [AppComponent],
  providers: [
    provideHttpClient(
      withInterceptors([credentialsInterceptor, retryInterceptor, sectionLoadingInterceptor, errorInterceptor])
    ),
    provideAnimations()
  ]
})
export class AppModule { }
