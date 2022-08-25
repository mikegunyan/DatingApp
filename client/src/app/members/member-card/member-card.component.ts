import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { Member } from 'src/app/_models/member';
import { Pagination } from 'src/app/_models/pagination';
import { MembersService } from 'src/app/_services/members.service';
import { PresenceService } from 'src/app/_services/presence.service';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css']
})
export class MemberCardComponent implements OnInit {
  @Input() member: Member;
  @Input() isLiked: boolean;
  @Input() predicate = 'liked';
  @Input() pageNumber = 1;
  @Input() pageSize = 10;
  @Output() newMembers = new EventEmitter<Member[]>();

  constructor(private memberService: MembersService, private toastr: ToastrService, public presence: PresenceService) { }

  ngOnInit(): void {
  }

  addLike(member: Member) {
    this.memberService.addLike(member.username).subscribe(() => {
      this.toastr.success('You have liked ' + member.knownAs);
    })
  }

  deleteLike(member: Member) {
    this.memberService.deleteLike(member.id).subscribe(() => {
      this.toastr.success('You have unliked ' + member.knownAs);
      this.memberService.getLikes(this.predicate, this.pageNumber, this.pageSize).subscribe(response => {
        this.newMembers.emit(response.result);
      })
    })
  }
}
