import { Component, signal, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface ChatMessage {
  id: number;
  text: string;
  sender: 'user' | 'bot';
  time: Date;
}

@Component({
  selector: 'app-chatbot',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chatbot.component.html',
  styleUrl: './chatbot.component.css'
})
export class ChatbotComponent implements AfterViewChecked {
  @ViewChild('messagesContainer') private messagesContainer!: ElementRef;

  isOpen = signal(false);
  isTyping = signal(false);
  inputText = signal('');
  messages = signal<ChatMessage[]>([
    {
      id: 0,
      sender: 'bot',
      text: 'Здраво! Ја сам еУправа асистент. Могу вам помоћи са питањима о регистрацији возила, потребним документима и поступцима. Поставите питање!',
      time: new Date()
    }
  ]);

  private nextId = 1;

  toggle() {
    this.isOpen.update(v => !v);
  }

  onInput(event: Event) {
    this.inputText.set((event.target as HTMLInputElement).value);
  }

  sendMessage() {
    const text = this.inputText().trim();
    if (!text) return;

    this.messages.update(msgs => [...msgs, {
      id: this.nextId++,
      sender: 'user',
      text,
      time: new Date()
    }]);
    this.inputText.set('');

    this.isTyping.set(true);
    setTimeout(() => {
      this.isTyping.set(false);
      this.messages.update(msgs => [...msgs, {
        id: this.nextId++,
        sender: 'bot',
        text: this.getResponse(text),
        time: new Date()
      }]);
    }, 900 + Math.random() * 600);
  }

  onKeyDown(event: KeyboardEvent) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  private getResponse(input: string): string {
    const q = input.toLowerCase();

    if (q.includes('документ') || q.includes('папир') || q.includes('шта треба') || q.includes('потреб')) {
      return 'За регистрацију возила потребно је: саобраћајна дозвола, полиса осигурања, потврда о техничком прегледу, доказ о власништву (купопродајни уговор) и лична карта власника.';
    }
    if (q.includes('цена') || q.includes('цијена') || q.includes('такса') || q.includes('колико кошта') || q.includes('плат')) {
      return 'Таксе за регистрацију возила зависе од врсте возила, носивости и старости. Информације о тачним износима такси доступне су на сајту МУП-а или на шалтерима.';
    }
    if (q.includes('продуж') || q.includes('обнов') || q.includes('истек')) {
      return 'Продужење регистрације можете извршити преко еУправе избором услуге "Продужење регистрације". Потребна је важећа полиса осигурања и потврда о техничком прегледу.';
    }
    if (q.includes('пренос') || q.includes('промена власник') || q.includes('продаја возила')) {
      return 'Пренос власништва покреће тренутни власник у профилу > Детаљи возила > Пренос власништва. Нови власник добија захтев и прихвата га. Потребан је купопродајни уговор.';
    }
    if (q.includes('таблиц') || q.includes('регистарска')) {
      return 'Промену регистарских таблица можете захтевати преко услуге "Промена регистарских таблица" на еУправи. Потребно је навести разлог промене.';
    }
    if (q.includes('технички') || q.includes('преглед')) {
      return 'Технички преглед возила обавља се у овлашћеним станицама за технички преглед. Потврда о прегледу је обавезна при регистрацији и продужењу.';
    }
    if (q.includes('одјав') || q.includes('брис')) {
      return 'Одјаву возила можете извршити преко услуге "Одјава возила" на еУправи. Регистарске таблице морају бити враћене надлежној јединици МУП-а.';
    }
    if (q.includes('регистрацј') || q.includes('регистрација') || q.includes('упис') || q.includes('регистров')) {
      return 'Упис возила у регистар подноси се преко еУправе. Попуните захтев, приложите потребне документе (саобраћајна дозвола, осигурање, технички преглед) и чекајте одобрење администратора.';
    }
    if (q.includes('осигурање')) {
      return 'Важећа полиса обавезног осигурања од аутоодговорности (АО) обавезна је за регистрацију и продужење. Осигурање се закључује код овлашћених осигуравајућих друштава.';
    }
    if (q.includes('рок') || q.includes('колико дуго') || q.includes('трај')) {
      return 'Регистрација возила важи 12 месеци. Можете је продужити до 30 дана пре истека. Возило ће бити означено упозорењем када регистрација ускоро истиче.';
    }
    if (q.includes('помоћ') || q.includes('help') || q.includes('шта умеш') || q.includes('шта можеш')) {
      return 'Могу вам помоћи са: регистрацијом возила, потребним документима, продужењем регистрације, преносом власништва, одјавом возила, таксама и роковима. Поставите конкретно питање!';
    }
    if (q.includes('хвала') || q.includes('хвала лепо') || q.includes('ок') || q.includes('разумем')) {
      return 'Нема на чему! Ако имате додатних питања, слободно питајте.';
    }

    return 'Нисам сигуран у вези са тим питањем. Можете се обратити шалтеру МУП-а или позвати инфо линију. Да ли желите да знате нешто о регистрацији, документима или поступцима?';
  }

  formatTime(date: Date): string {
    return date.toLocaleTimeString('sr-RS', { hour: '2-digit', minute: '2-digit' });
  }

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  private scrollToBottom() {
    try {
      const el = this.messagesContainer?.nativeElement;
      if (el) el.scrollTop = el.scrollHeight;
    } catch {}
  }
}
