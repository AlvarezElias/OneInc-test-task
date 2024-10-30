import { Component, OnDestroy, OnInit } from '@angular/core';
import { WebSocketService } from '../../services/web-socket.service';
import { debounceTime, Subject, Subscription } from 'rxjs';
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
  private openSubscription!: Subscription;
  private isConnected: boolean = false;
  private inputSubject: Subject<string> = new Subject();

  constructor(private webSocketService: WebSocketService) {}

  ngOnInit(): void {
    this.inputSubject.pipe(debounceTime(1200)).subscribe((value) => {
      this.connectAndSendMessage(value);
    });

    this.messageSubscription = this.webSocketService.messages$.subscribe((message) => {
      if(message == "FINISH_PROCESS") {
        this.isDisabledInputText = false;
      } else {
        this.encodedText += message;
      }
    });
  }
  
  onInputChange(value: string): void {
    this.inputSubject.next(value);
  }

  connectAndSendMessage(message: string): void {
    if (this.isConnected) {
      this.sendMessage(message);
      return;
    }

    if(this.isConnected){
      this.sendMessage(this.inputText);
    } else { 
      this.connectWebSocket();

      this.sendMessage(this.inputText);  

    }
  }

  connectWebSocket(): void {
    if(this.inputText){
      this.isDisabledInputText = true;
    
      this.webSocketService.connect();
      
      this.isConnected = true;  

      this.openSubscription = this.webSocketService.open$.subscribe((status) => {
        this.sendMessage(this.inputText);
      });

    }
  }

  sendMessage(value: string): void {
    if(this.isConnected){
      this.isDisabledInputText = true;
      this.encodedText = "";
      this.webSocketService.sendMessage(value);
    }
  }

  cancelConnection(): void {
    this.webSocketService.cancel();
    this.isConnected = false;
    this.isDisabledInputText = false;
  }

  ngOnDestroy(): void {
    this.webSocketService.cancel();
    this.inputSubject.unsubscribe();
    if(this.openSubscription)
        this.openSubscription.unsubscribe();
    if(this.messageSubscription)
      this.messageSubscription.unsubscribe();
  }
}
