-- +goose Up
-- +goose StatementBegin
create table course_files
(
    course_id  bigint not null,
    file_id    bigint not null,
    teacher_id bigint not null,

    primary key (course_id, file_id)
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table course_files;
-- +goose StatementEnd
