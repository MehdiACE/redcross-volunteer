import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  title = 'redcross-manager-client';

  constructor(private translate: TranslateService) {
    // Set default language
    this.translate.setDefaultLang('fr');
    // Use French by default
    this.translate.use('fr');
  }

  ngOnInit(): void {
    // Optional: detect browser language
    const browserLang = this.translate.getBrowserLang();
    if (browserLang && ['en', 'fr'].includes(browserLang)) {
      this.translate.use(browserLang);
    }
  }
}
