-- +goose Up
-- +goose StatementBegin
create table teacher_homework_approvals
(
    id                bigserial primary key not null,
    teacher_id        bigint                not null,
    group_homework_id bigint                not null,
    creation_time     timestamptz           not null
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table teacher_homework_approvals;
-- +goose StatementEnd
