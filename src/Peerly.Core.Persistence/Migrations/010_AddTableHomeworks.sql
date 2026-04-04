-- +goose Up
-- +goose StatementBegin
create table homeworks
(
    id                    bigserial primary key not null,
    course_id             bigint                not null,
    group_id              bigint,
    teacher_id            bigint                not null,
    name                  text                  not null,
    status                text                  not null,
    amount_of_reviewers   int                   not null,
    description           text,
    checklist             text                  not null,
    deadline              timestamptz           not null,
    review_deadline       timestamptz           not null,
    discrepancy_threshold int                   not null,
    creation_time         timestamptz           not null,
    update_time           timestamptz
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table homeworks;
-- +goose StatementEnd
