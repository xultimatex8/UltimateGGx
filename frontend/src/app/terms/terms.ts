import { afterNextRender, Component } from '@angular/core';
import { ScrollUtil } from '../shared/utils/scroll-util';

interface TermsSection {
  id: string;
  title: string;
  paragraphs: string[];
}

@Component({
  selector: 'app-terms',
  imports: [],
  templateUrl: './terms.html',
})
export class Terms {
  protected readonly lastUpdated = 'August 6, 2026';
  protected readonly privacyEmail = 'agmdeveloper@outlook.com';

  readonly ScrollUtil = ScrollUtil;

  protected readonly sections: TermsSection[] = [
    {
      id: 'about',
      title: '1. About this project',
      paragraphs: [
        'UltimateGGx is an independent, fan-made project for exploring and analyzing League of Legends match data. It is not an official Riot Games product and is not affiliated with, endorsed by, or sponsored by Riot Games, Inc.',
        'UltimateGGx was created under Riot Games\u2019 "Legal Jibber Jabber" policy using assets owned by Riot Games. Riot Games does not endorse or sponsor this project.',
        'The project is provided free of charge, for informational and educational purposes. UltimateGGx does not charge for access and does not use paywalled content. Any future monetization would only be introduced in compliance with Riot Games\u2019 API Terms and with Riot\u2019s prior written approval.',
        'UltimateGGx depends on Riot Games APIs and may be temporarily unavailable due to maintenance, Riot API outages, rate limits, or other technical issues.',
      ],
    },
    {
      id: 'riot-notice',
      title: '2. Riot Games legal notice',
      paragraphs: [
        'UltimateGGx is not endorsed by Riot Games and does not reflect the views or opinions of Riot Games or anyone officially involved in producing or managing Riot Games properties. Riot Games and all associated properties are trademarks or registered trademarks of Riot Games, Inc.',
        'League of Legends © Riot Games, Inc.',
        'All match data displayed is retrieved from the official Riot Games API and remains the property of Riot Games, Inc. Use of this data is subject to the Riot Games API Terms of Service.',
      ],
    },
    {
      id: 'use-of-service',
      title: '3. Use of the service',
      paragraphs: [
        'By using UltimateGGx you agree to use it only for lawful purposes and in a way that does not infringe the rights of, or restrict or inhibit the use and enjoyment of, this site by anyone else.',
        'You may not attempt to disrupt the service, scrape it at abusive rates, or use it to circumvent Riot Games\u2019 own rate limits or terms of use.',
        'Match reconstructions and visualizations are generated from Riot Games timeline data and other publicly available match information. Some game states are reconstructed and may not exactly match the original in-game state.',
        'The "counterfactual simulation" features (what-if scenarios, alternate timelines, win-probability estimates) are approximations built from public match timeline data. They are illustrative and should not be treated as an exact reconstruction of what would have happened in a real match.',
      ],
    },
    {
      id: 'data-privacy',
      title: '4. Data and privacy',
      paragraphs: [
        'UltimateGGx only stores publicly available match and summoner data obtained through the Riot Games API (e.g. summoner names, ranks, match history, timelines). No passwords, payment information, or Riot account credentials are ever collected or stored.',
        'Match and player account data is cached to improve performance and reduce unnecessary requests to the Riot Games API. Cached data is refreshed on request and may be removed upon request where applicable.',
        'In compliance with GDPR-aligned Riot Games policies, if Riot Games notifies us that a player has requested deletion of their data, the corresponding cached data associated with that player will be deleted from our systems.',
      ],
    },
    {
      id: 'warranty',
      title: '5. Disclaimer of warranty',
      paragraphs: [
        'UltimateGGx is provided "as is" and "as available", without warranties of any kind, express or implied, including accuracy, completeness, or fitness for a particular purpose.',
        'Match reconstructions, player positions, scoreboard states, and simulated outcomes are approximate. Player positions and scoreboard values are reconstructed from the nearest available Riot Games timeline snapshots and may differ from the exact in-game state at a given moment. UltimateGGx does not guarantee the accuracy or completeness of reconstructed or simulated game states.',
      ],
    },
    {
      id: 'liability',
      title: '6. Limitation of liability',
      paragraphs: [
        'To the fullest extent permitted by law, the creator of UltimateGGx is not liable for any indirect, incidental, or consequential damages arising from the use of, or inability to use, this service.',
        'The creator of UltimateGGx is not responsible for decisions or conclusions made based on analyses, visualizations, reconstructed game states, or simulated outcomes provided by the service.',
      ],
    },
    {
      id: 'changes',
      title: '7. Changes to these terms',
      paragraphs: [
        'These terms may be updated from time to time as the project evolves. Continued use of UltimateGGx after changes are published constitutes acceptance of the revised terms.',
      ],
    },
    {
      id: 'contact',
      title: '8. Contact',
      paragraphs: [
        'This project has two separate contact points, please use the right one:',
      ],
    },
  ];

  constructor() {
    afterNextRender(() => {
      window.scrollTo({ top: 0, left: 0, behavior: 'smooth', });
    });
  }
}