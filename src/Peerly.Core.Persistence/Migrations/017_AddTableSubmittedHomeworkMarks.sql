-- +goose Up
-- +goose StatementBegin
create table submitted_homework_marks
(
    submitted_homework_id bigint primary key not null,
    reviewers_mark         int                not null,
    teacher_mark           int,
    teacher_id             bigint,
    creation_time          timestamptz        not null,
    update_time            timestamptz
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table submitted_homework_marks;
-- +goose StatementEnd
