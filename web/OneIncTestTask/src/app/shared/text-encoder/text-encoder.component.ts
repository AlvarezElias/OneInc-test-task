import { Component, OnDestroy, OnInit } from '@angular/core';
import { WebSocketService } from '../../services/web-socket.service';
import { Subscription } from 'rxjs';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-text-encoder',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './text-encoder.component.html',
  styleUrl: './text-encoder.component.css'
})
export class TextEncoderComponent implements OnInit, OnDestroy {
  inputText: string = '';
  encodedText: string = '';
  isDisabledInputText: boolean = false;
  private messageSubscription!: Subscription;
  private isConnected: boolean = false;

  constructor(private webSocketService: WebSocketService) {}

  ngOnInit(): void {

  }

  connectWebSocket(): void {
    // deshabilitar input
    if(!this.isConnected) {
      this.webSocketService.connect(this.inputText);
      this.isConnected = true;
      this.isDisabledInputText = true;
      this.messageSubscription = this.webSocketService.messages$.subscribe((message) => {
        this.encodedText += message;
      });

    }
    // habilitar input
  }

  sendMessage(): void {
    console.log("cambio");
    if(this.isConnected){
      this.webSocketService.close();
      this.encodedText = "";
      //this.webSocketService.sendMessage(this.inputText);
      this.connectWebSocket();
    }
  }

  closeConnection(): void {
    this.webSocketService.close();
    this.encodedText = "";
    this.isConnected = false;
    this.isDisabledInputText = false;
  }

  ngOnDestroy(): void {
    this.webSocketService.close();
    if(this.messageSubscription)
      this.messageSubscription.unsubscribe();
  }
}
