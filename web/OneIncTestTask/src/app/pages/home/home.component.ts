import { Component } from '@angular/core';
import { TextEncoderComponent } from "../../shared/text-encoder/text-encoder.component";

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [TextEncoderComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {

}
